import { dateFormat } from "../methods/Enum.min.js";

/**
 * Classe d'encapsulation pour le formatage des dates.
 * */
export default class eDateFnsHelper {

    /**
     * Initialise l'objet avec la date données et le format prévu.
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
     * Retourne une chaine de charactere représentant la date
     * reformatée suivant la chaine de formatage passée en paramètre
     * du constructeur.
     * @returns {string} une chaine représentant la date reformatée.
     * */
    getFormatedDate() {

        if (this.strDateToConvert == "" || this.strDateFormat == "")
            return "";

        let newFormat = moment(this.strDateToConvert, Object.values(dateFormat), this.strCulture);
        return newFormat.format(dateFormat[this.strDateFormat]);         
    }
}