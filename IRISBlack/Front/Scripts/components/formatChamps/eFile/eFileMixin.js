import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { updateMethod, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { selectValue, onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { getTabDescid } from "../../eMainMethods.js?ver=803000";

/**
 * Mixin commune aux composants eFile.
 * */
export const eFileMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            oFileNameEditor: '',
            oPopupObj: '',
            advancedDialog: null,
            oFileNameEditor: "",
            sourceElement: null,
            headerElement: null,
            isEditing: false,
            parentPopup: null,
            selectedValues: null,
            selectedLabels: null,
            currentValues: null,
            currentLabels: null,
            target: null,
            modif: false,
            icon: false
        };
    },
    computed: {
        msgHover: function () {
            if (this.icon)
                //return 'Ouvrir ou télécharger le fichier'
                return this.getRes(8070)
            else if (!this.icon && this.IsDisplayReadOnly)
                return this.getRes(2477)
            else
                //return 'Ajouter/Changer un fichier'
                return this.getRes(7393)
        }
    },
    mounted() {
        this.displayInformationIco();
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        updateListVal,
        getTabDescid,
        showTooltip,
        hideTooltip,
        onUpdateCallback,
        selectValue,
        updateMethod,
        IsValueValid(sValueToTest, sValueToTestRawFormat) {

            var srcElement = this.GetSourceElement(this.target)
            if (srcElement == null)
                return false;

            sValueToTest = sValueToTest + "";
            if (sValueToTest == '')
                return true;
            switch (srcElement.getAttribute("eaction")) {
                case "LNKMAIL":
                    that.SpecificErrorMessge = this.getRes(1023).replace('<EMAIL>', '[' + sValueToTest + ']');

                    // HLA - Envoi Emiailing avec plusieurs emails séparés par ";" dans le champ email - On considère la première adresse mail comme principale et les suivantes en CC - #39682
                    var mailValid = true;
                    forEach(sValueToTest.split(';'), function (param) {
                        var mail = trim(param);
                        if (mail.length != 0)
                            mailValid = mailValid && eValidator.isEmail(mail);
                    });

                    return mailValid;
                case "LNKNUM":
                    that.SpecificErrorMessge = this.getRes(673);
                    return isValidNumber(sValueToTest);

                case "LNKFREETEXT":

                    bIsDate = (getAttributeValue(that.headerElement, "frm") == FLDTYP.DATE);
                    if (bIsDate) {

                        that.SpecificErrorMessge = this.getRes(1304) + " : " + eDate.CultureInfoDate();

                        //  that.SpecificErrorMessge = this.getRes(1023).replace('<EMAIL>', '[' + sValueToTest + ']');
                        return eValidator.isDateJS(sValueToTestRawFormat);
                    }
                    return true;
                case "LNKGEO":
                    return eValidator.isGeo(sValueToTest);
                    break;
            }



            return true;
        },
        GetSourceElement(evt) {
            this.parentPopup = new ePopup('oPopupObj', 220, 250, 0, 0, document.body, false);
            this.sourceElement = evt.target;
            if (this.sourceElement)
                return this.sourceElement;
            else
                return this.parentPopup.sourceElement;
        },
        //Indique si l'objet champ est déjà en cours d'édition ( isEditing à true si onclick et a false si onblur)
        setIsEditing(bEditing) {
            this.isEditing = bEditing;
        },

        /*********KHA validation de champs de type fichier***********/
        /* ELAIZ - J'ai mis async sur la méthode car je récupère l'import d'axios avec l'opérateur await devant*/
        async validFileField(evt, objEditor) {

            var aReturnValue = objEditor.advancedDialog.getIframe().getSelectedFiles();

            var oDoc = objEditor.advancedDialog.getIframe().document;

            var divList = oDoc.getElementById("divLstFiles");
            if (divList != null) {
                var lstDiv = divList.children;
                for (var i = 0; i < lstDiv.length; i++) {
                    var ref = lstDiv[i].getAttribute("ref");
                    if (ref != null) {
                        objEditor.ForceRefresh = ref == "1" ? true : false;
                        break;
                    }
                }
            }

            //objEditor.advancedDialog.hide();
            objEditor.selectedValues = new Array();
            objEditor.selectedLabels = new Array();

            for (i = 0; i < aReturnValue.length; i++) {
                objEditor.selectValue(aReturnValue[i], aReturnValue[i], true);
            }

            //this.validateFile();

            /*ELAIZ - Finalement j'ai décidé de ne pas appeler la méthode validateFile que j'ai récupérée de validateLnkFile 
             dans eRelation, j'envoie la data à la suite et ensuite je ferme la modale */

            var TabDescId;
            if (this.propSignet == undefined) {
                TabDescId = this.getTab;
            } else {
                TabDescId = this.propSignet.DescId;
            }

            updateMethod(objEditor, this.selectedValues[0], undefined, undefined, this.dataInput);

            objEditor.advancedDialog.hide();

        },
        openFilesMgrDialog(evt) {
            var that = this;
            this.target = evt;
            //this.headerElement = document.querySelector("tp='"+(getAttributeValue(evt.target, "divdescid")+"'"));
            this.headerElement = this.$parent.$el.querySelector('[divdescid="' + this.dataInput.DescId + '"]');
            if (this.advancedDialog != null) {
                try {
                    this.advancedDialog.hide();
                }
                catch (e) {
                    debugger;
                }
            }
            //var nTab = "[" + this.getTab + "]"
            var nTab = "[" + this.getTabDescid(this.dataInput.DescId) + "]"
            this.advancedDialog = new eModalDialog(this.getRes(103), 0, 'eFieldFiles.aspx', 850, 500);
            this.advancedDialog.addParam("descid", this.dataInput.DescId, "post");
            this.advancedDialog.addParam("folder", nTab, "post");
            this.advancedDialog.addParam("files", this.dataInput.Value, "post");
            /* ELAIZ- je ne pense pas avoir besoin de ce param ( multiple) */
            //this.advancedDialog.addParam("mult", this.multiple ? "1" : "0", "post");

            this.advancedDialog.show();

            var myFunct = (function (evt, obj) { return function () { that.validFileField(evt, obj); } })(evt, this);
            this.advancedDialog.addButton(this.getRes(5003), myFunct, "button-green", "", "ok"); //Valider


        },
        goAction(evt) {
            if (!evt.target.classList.contains("targetIsTrue")) {

                this.openFilesMgrDialog(evt);
            }
        },
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}