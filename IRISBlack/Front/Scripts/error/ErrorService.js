/** classe de service pour la gestion des erreurs */
export default class ErrorService {

    /**
     * constructeur pour les erreurs Vuejs et non Vuejs
     * @param {any} msg
     * @param {any} src
     * @param {any} linenum
     * @param {any} colnum
     * @param {any} error
     */
    constructor(error, src, msg, linenum, colnum) {
        this.error = error;
        this.vuecpt = src;
        this.information = msg;
        this.line = linenum;
        this.column = colnum;
    }

    /**
     * Initialiseur static de la classe service pour les erreurs et les warnings vuejs.
     * @param {any} err
     * @param {any} vm
     * @param {any} info
     */
    static initErrorServiceVue(err, vm, info) {
        return new ErrorService(err, vm, info);
    }

    /**
     * Initialiseur static de la classe service pour les erreurs autres.
     * @param {any} msg
     * @param {any} src
     * @param {any} linenum
     * @param {any} colnum
     * @param {any} error
     */
    static initErrorServiceJS(msg, src, linenum, colnum, error) {
        return new ErrorService(error, src, msg, linenum, colnum);
    }

    /**
     * Affiche toutes les informations sur l'erreur en cours.
     */
    ToString() {
        console.log("****************************************************************");
        console.log(new Date().toLocaleString("fr-fr"));
        console.log(this.error);
        console.log(this.vuecpt);
        console.log(this.information);
        console.log(this.line);
        console.log(this.column);
        console.log("****************************************************************");
    }
}