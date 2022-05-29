import { FieldType } from './Enum.js?ver=803000';
import { updateMethod } from './eComponentsMethods.js?ver=803000'

/**
 * Va executer toutes les vérifications idoines, séquentiellement.
 * @param {any} pattern la pattern de vérification.
 * @param {any} event l'évent qui est provoqué par l'utilisateur.
 * @param {any} oldValue l'ancienne valeur du composant.
 * @param {any} ctx le contexte appelant.
 * @param {object} inputData l'ensemble des données de la fiche.
 * @param {object} additionalUpdateData paramètres de mise à jour additionnels à passer à la fonction updateMethod, qui les fusionnera avec ceux qu'elle construit à la base
 */
export function verifComponentModal(pattern, valEntry, oldValue, ctx, inputData, additionalUpdateData) {
    let TimeOutModif = 250;

    if (valEntry == oldValue && valEntry != "") {
        if (pattern) {
            verifRegexOnBlankModal(pattern, valEntry, ctx, oldValue, inputData);
            if (ctx.bRegExeSuccess)
                setTimeout(function () { ctx.modif = false; }, TimeOutModif);
            return;
        }

        ctx.bDisplayPopup = false;
        setTimeout(function () { ctx.modif = false; }, TimeOutModif);
        return;
    }
    else if (valEntry == oldValue && valEntry === "") {
        ctx.bDisplayPopup = false;
        setTimeout(function () { ctx.modif = false; }, TimeOutModif);
        return;
    }

    if (valEntry === "")
        verifCharacterModal(valEntry, ctx, oldValue);
    else if (pattern)
        verifRegexModal(pattern, valEntry, ctx, oldValue, inputData);
    else {
        updateMethod(ctx, valEntry, oldValue, additionalUpdateData, inputData);
    }
}


/**
 * Permet de vérifier, en sortant du champs que la valeur est bien correcte.
 * Et éventuellement d'enregistrer l'adresse.
 * La regex vient de Monsieur W3C.
 * @param {string} regex l'expression régulière de vérification.
 * @param {object} elem un objet représetant l'objet courant modifié.
 * @param {object} that context de l'appelant.
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur
 */
export function verifRegexModal(regex, valEntry, that, oldValue, inputData) {
    verifRegexOnBlankModal(regex, valEntry, that, inputData);

    if (!that.bRegExeSuccess)
        return;

    try {
        updateMethod(that, valEntry, oldValue, undefined, inputData);
    } catch (e) {
        papa.classList.add("border-error");
        that.bDisplayPopup = true;
    }
}

/**
 * Permet de valider une adresse mail, juste ça.
 * @param {string} regex l'expression régulière de vérification.
 * @param {any} valEntry  un objet représetant l'objet courant.
 * @param {object} that context de l'appelant.
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur
 */
export function verifRegexOnBlankModal(regex, valEntry, that, oldValue) {

    if (valEntry == "") {
        that.bDisplayPopup = false;
        return;
    }

    var papa = getContainerElement(that);
    RemoveBorderSuccessError(papa);
    papa.classList.remove("emptyBase");
    SetErrorMessage(that.dataInput, '');

    that.bRegExeSuccess = regex.test(valEntry);
    that.dataInput.Value = valEntry;

    if (!that.bRegExeSuccess) {
        that.bDisplayPopup = true;
        papa.classList.add("border-error");
        SetErrorMessage(that.dataInput, that.messageError); // #80 517 - ajout du message d'erreur par défaut du composant (utilisé en cas d'incohérence de saisie) sur dataInput pour affichage en infobulle
        if (that.propListe)
            papa.classList.add("emptyBase"); // remplace l'icône "Retour" par l'icône par défaut pour le mode Liste
    }
}

/**
 * Empêcher de vider les rubriques obligatoires (Tache 1907 - US 1142)
 * @param {any} elem l'élement qui envoie la demande.
 * @param {any} that le contexte d'appel (this)
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur
 */
export function verifCharacterModal(valEntry, that, oldValue, inputData) {


    var papa = that.$parent.$el.querySelector('div[divdescid="' + that.dataInput.DescId + '"]');

    if (!that.dataInput.Required && that.dataInput.Format != FieldType.Catalog)
        updateMethod(that, valEntry, oldValue, inputData);
    else if (!that.dataInput.Required && that.dataInput.Format == FieldType.Catalog) {
        updateMethod(that, that.selectedValues.join(';'), undefined, undefined, that.dataInput);
    }


    if (papa != null && that.dataInput.Required) {
        if (valEntry != "" && that.dataInput.Format != FieldType.Catalog) {
            that.bEmptyDisplayPopup = false;
            papa.classList.remove("border-error");
            papa.classList.remove("emptyBase");
            SetErrorMessage(that.dataInput, '');
            that.dataInput.Value = valEntry;
            updateMethod(that, valEntry, oldValue, inputData);
        } else {
            that.bEmptyDisplayPopup = true;
            papa.classList.add("border-error");

            // US #1854 - Demande #80 517 et Tâche #2257 - On matérialise les champs avec le même DescID sur la même page en erreur, mais uniquement en mode Fiche
            if (!that.propListe)
                [].slice.call(that.$parent.$el.querySelectorAll(`[divdescid="${that.dataInput.DescId}"]`)).map(x => x.classList.add("border-error"));
            // Sinon, en mode Liste, on remonte le message d'erreur pour affichage au survol (via showTooltip)
            else {
                SetErrorMessage(that.dataInput, that.getRes(2471)); // Cette rubrique est obligatoire //that.messageError
                papa.classList.add("emptyBase"); // remplace l'icône "Retour" par l'icône par défaut pour le mode Liste
            }
        }
        return;
    }

}
