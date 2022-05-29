import { dateFormat } from "../methods/Enum.min.js";

/**
 * Classe d'encapsulation pour le formatage des dates.
 * */
export default class eDateFnsHelper {

    /**
     * Initialise l'objet avec la date donn�es et le format pr�vu.
     * @param {any} strDt
     * @param {any} strFormat
     * @param {any} strClt
     */
    constructor(strDt, strFormat, strClt) {
        this.strDateToConvert = strDt;
        this.strDateFormat = strFormat;
        this.strCulture = strClt;
    }

    /** 
     * Retourne une chaine de charactere repr�sentant la date
     * reformat�e suivant la chaine de formatage pass�e en param�tre
     * du constructeur.
     * @returns {string} une chaine repr�sentant la date reformat�e.
     * */
    getFormatedDate() {

        if (this.strDateToConvert == "" || this.strDateFormat == "")
            return "";

        let newFormat = moment(this.strDateToConvert, Object.values(dateFormat), this.strCulture);
        return newFormat.format(dateFormat[this.strDateFormat]);         
    }
}