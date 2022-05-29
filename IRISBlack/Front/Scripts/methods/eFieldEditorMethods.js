import { updateMethod } from './eComponentsMethods.js?ver=803000';
import EventBus from "../bus/event-bus.js?ver=803000";

/**
 * Ajoute une donnée à une liste, ou la retire.
 * C'est selon.
 * @param {any} value la valeur à ajouter à une liste de valeurs
 * @param {any} label le label à ajouter à une liste de labels.
 * @param {any} selected l'ajout ou la suppression.
 */
export function selectValue(value, label, selected) {
    if (selected) {
        if (this.selectedValues.length == 0 || this.selectedValues.indexOf(value) == -1) {
            this.selectedValues.push(value);
            this.selectedLabels.push(label);
        }
    }
    else {
        if (this.selectedValues.length > 0 && this.selectedValues.indexOf(value) > -1) {
            this.selectedValues.splice(this.selectedValues.indexOf(value), 1);
            this.selectedLabels.splice(this.selectedLabels.indexOf(label), 1);
        }
    }
};

/**
 * Un callback qui est appelé après envoi et réception de la MAJ vers/depuis le backend
 * @param {bool} e le succès ou l'échec.
 * @param {object} objVal objet représentant les valeurs et les labels à afficher.
 * @param {any} err les erreurs ou exception
 * @param {any} updateAreas zone à aussi mettre à jour ( même champs dans une autre zone )
 * */
export async function onUpdateCallback(e, objVal, err, updateAreas) {
    // Succès : on renvoie les données passées en entrée
    if (e) {
        Vue.nextTick(function () {
            let options = {
                reloadSignet: updateAreas?.updateBkmArea,
                reloadHead: updateAreas?.updateSummaryArea,
                reloadAssistant: updateAreas?.updateWizardArea,
                reloadAll: updateAreas?.updateAllAreas,
                reloadDetail:updateAreas?.updateDetailsArea
            }
            if (updateAreas?.needToUpdate) {
                EventBus.$emit('emitLoadAll', options);
            }
        });
        return objVal;
    }
    // Erreur : on renvoie l'exception passée en entrée
    else {
        return err;
    }
}


/**
 * Bouton annuler d'une popup.
 * @param {any} trgId l'identifiant de l'élément cible.
 */
export function cancelCatGenericIris(trgId) {
    if (this.partOfAfterCancel && typeof this.partOfAfterCancel == "function") {
        this.partOfAfterCancel(this.catalogDialog, trgId);
    }
    this.modif = false;

    this.catalogDialog.hide();
}

/**
 * Bouton valider d'une popup.
 * @param {any} fnUpdate la fonction de mise à jour
 */
export function validateCatGenericIris(fnUpdate) {
    var oFrm = this.catalogDialog.getIframe();

    var selectedListValues = oFrm.selectedListValues;
    if (typeof (selectedListValues) == 'undefined' && typeof (oFrm.eC) != 'undefined')
        selectedListValues = oFrm.eC.getSelectedListId();

    var tabSelectedValues = new Array();
    var tabSelectedLabels = new Array();

    var selectedIDs = "";

    if (oFrm.eC && oFrm.eC.treeview && oFrm.eC.selectedListValuesDico) {
        //Cas particulier pour treeview


        var keys = oFrm.eC.selectedListValuesDico.Keys;
        for (var i = 0; i < keys.length; i++) {

            var nId = keys[i];
            var sVal = oFrm.eC.selectedListValuesDico[nId];

            tabSelectedValues.push(nId);
            tabSelectedLabels.push(sVal);
            if (selectedIDs != "") {
                selectedIDs = selectedIDs + ";";
            }
            selectedIDs += nId;
        }
    }
    else {

        for (var i = 0; i < selectedListValues.length; i++) {
            if (selectedListValues[i] == "")    //le getElementById vide plante sous IE8
                continue;
            var oItem = oFrm.document.getElementById(selectedListValues[i]);

            if (!oItem)
                continue;
            var label = '';
            if (oItem.getAttribute('ednval') == "") {
                label = '';
            }
            else {
                var id_Lbl = selectedListValues[i].replace('val', 'lbl');
                id_Lbl = id_Lbl.replace('_sel', '');
                label = oFrm.document.getElementById(id_Lbl).innerText || oFrm.document.getElementById(id_Lbl).textContent; // #42895 CRU : On affiche la valeur texte et non innerHtml
                tabSelectedValues.push(oItem.getAttribute('ednval'));
                tabSelectedLabels.push(label);
            }

            if (oItem.getAttribute('ednid') != "") {
                if (selectedIDs != "") {
                    selectedIDs = selectedIDs + ";";
                }
                selectedIDs = selectedIDs + oItem.getAttribute('ednid');
            }
        }
    }

    this.selectedValues = new Array();
    this.selectedLabels = new Array();
    var cntFld = 0;
    for (cntFld = 0; cntFld < tabSelectedValues.length; cntFld++)
        this.selectValue(tabSelectedValues[cntFld], tabSelectedLabels[cntFld], true);

    /** TODO : On verra plus tard pour ce truc-là, hein ... Parce que c'est pas tout, mais, bon, voilà, quoi... G.L */
    //validate(); // Enregistrement des valeurs sélectionnées en base et de son rafraichissement


    //ELAIZ - je rajoute cette vérification pour éviter l'update si la rubrique est obligatoire et que l'on enlève la dernière valeur du cat
    if (this.dataInput && this.dataInput.Required && this.selectedValues.length < 1) {
        this.bEmptyDisplayPopup = true;
        this.$el.parentElement.parentElement.classList.add("border-error");
        [].slice.call(this.$parent.$el.querySelectorAll(`[divdescid="${this.dataInput.DescId}"]`)).map(x => x.classList.add("border-error"));
        this.catalogDialog.hide();
        return false;
    }

    fnUpdate();

    this.catalogDialog.hide();
}


/**
 * A l'annulation de la popup utilisateur.
 * */
export function cancelAdvancedDialog() {
    this.advancedDialog.hide();
}

/**
 * A la validation de la popup utilisateur.
 * @param {any} fnUpdate la fonction de mise à jour.
 * */
export function validateUserDialog(fnUpdate) {

    var modalObject = null;
    var oFrame = null;

    modalObject = this.advancedDialog;
    if (modalObject)
        oFrame = modalObject.getIframe();

    if (modalObject && oFrame) {
        var strReturned = oFrame.GetReturnValue();
        modalObject.hide();
        var tabReturned = strReturned.split('$|$');
        var vals = tabReturned[0];
        var libs = tabReturned[1];

        this.selectValue(vals, libs, true);

        fnUpdate();
    }
    else
        alert("erreur : oFrame " + oFrame + " - modalObject " + modalObject + " - oFrame " + oFrame);
}

/**
 * ajustement des colonnes.
 * @param {any} sValSelector la colonne.
 * @param {any} nDefaultWidth la valeur par défaut pour sa taille.
 */
export function adjustColWidth(sValSelector, nDefaultWidth) {
    let nWidth = (nDefaultWidth) ? nDefaultWidth : 0;
    let oRule = getCssSelector("eCatalog.css", sValSelector);

    if (!oRule || oRule == null)
        return;

    let aLiVals = [...this.catalogDialog.getIframe().document.querySelectorAll(sValSelector)];

    if (aLiVals.length < 1) {
        return;
    }

    nWidth = Math.max(...aLiVals.map(li => li.offsetWidth));

    //oRule.style.width = nWidth + "px";
    //oRule.style.minWidth = nWidth + "px";

    aLiVals.forEach(li => li.style.width = nWidth + "px");
}

/**
 * récupère une classe dans une feuille de sytle et incrémente sa taille 
 * par une valeur déterminée.
 * @param {any} className le nom de la classe à récupérer.
 * @param {any} increment la valeur à incrémenter pour que la largeur soit cohérente.
 */
export function NewWidthCol(className, increment) {
    let oRule = getCssSelector("eCatalog.css", "li." + className);
    if (!oRule)
        return;

    let iWidth = getNumber(oRule.style.width.replace("px", ""));

    if (isNaN(iWidth))
        throw top._res_680;

    oRule.style.width = (iWidth + increment) + "px";
}

/**
 * Agrandit l'intérieur de la popup pour qu'elle s'adapte au containeur.
 * @param {any} eCEDValues les valeurs sélectionnées.
 * @param {any} tbCatVal les valeurs possibles
 * @param {any} bBtn y'a-t-il une entête aux colonnes.
 */
export function EnlargeColsIfNeeded (eCEDValues, tbCatVal, bBtn) {

    if (eCEDValues.scrollWidth <= tbCatVal.offsetWidth)
        return;

    // on agrandit le contenu pour qu'il s'ajuste au conteneur
    let aLiHead = [...tbCatVal.querySelectorAll("li[hd]>ul>li")];
    let increment = eCEDValues.scrollWidth - tbCatVal.offsetWidth;

    if (aLiHead.length > 0) {
        let nbCol = bBtn ? aLiHead.length - 1 : aLiHead.length;                              //-1 parce que la dernière colonne est celle des boutons
        increment = Math.floor(increment / nbCol);

        aLiHead.forEach(li => this.NewWidthCol(li.className, increment));
    }
    else {
        // catalogues V7 :
        this.NewWidthCol("maskwidth", increment - 1);
    }

}

/**
 * fonction qui ajuste les colonnes d'un catalogue.
 * */
export function adjustColsWidth() {
    let qFrame = this.catalogDialog.getIframe().document;

    this.adjustColWidth("li.valwidth", 150);
    this.adjustColWidth("li.maskwidth", 150);
    this.adjustColWidth("li.datawidth");
    this.adjustColWidth("li.idwidth");
    this.adjustColWidth("li.diswidth");

    let tbCatVal = qFrame.getElementById("tbCatVal");
    let eCEDValues = qFrame.getElementById("eCEDValues");
    if (tbCatVal && eCEDValues)
        this.EnlargeColsIfNeeded(eCEDValues, tbCatVal, true);

    //pour les catalogues multiples
    tbCatVal = qFrame.getElementById("tbCatSelVal");
    eCEDValues = qFrame.getElementById("eCEDSelValues");
    if (tbCatVal && eCEDValues)
        this.EnlargeColsIfNeeded(eCEDValues, tbCatVal, false);

}