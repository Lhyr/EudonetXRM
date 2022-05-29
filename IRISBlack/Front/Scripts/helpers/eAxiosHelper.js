/**
 * Classe d'encapsulation pour les appels Axios.
 * */
export default class eAxiosHelper {

    /**
     * Constructeur pour le helper
     * @param {string}  lien lien à utiliser pour l'envoi et la réception des données.
     * */
    constructor(lien) {
        /** On initialise les variables de classes ici. Parce que...Safari ne supporte pas les champs. */
        this.sLink = lien;
        this.callHelper;

        this.InitCallHelper();
    }

    /** 
     *  fonction, qui sur une promesse retourne 
     * les données comprises dans dans la réponse
     * d'une requete http. 
     * Si la réponse HTTP n'est pas valide (!=200, on renvoie un vide)
     * @param {HttpResult} lstObj reponse d'une requete HTTP.
     * @param {Function} clbck fonction de callback eventuelle.
     * @returns {Object} data
     * */
    ReturnResultAndCallback (lstObj, clbck) {

        if (lstObj.status < 200 || lstObj.status > 299)
            throw lstObj.status + " : " + lstObj.statusText;

        let data = lstObj.data;

        if (clbck && typeof clbck === "function")
            clbck();

        return data;
    }


    /** fonction qui initialise le Helper d'Axios.
     * @param {object} Helper, le helper pour l'initialisation.
     */
    InitCallHelper(Helper) {
        this.callHelper = !Helper ? axios : Helper;
    }


    /**
     * Permet d'envoyer des données à une URL distante, avec un objet,
     * suivant la méthode comprise dans l'objet.
     * En cas de besoin, l'objet est initialisé avec la méthode get et 
     * l'url lien.
     * @param {Object}  objData les données supplémentaires pour l'envoi.
     * @param {Function} clbck fonction de callback eventuelle.
     * @returns {JSon} les données contenues dans la réponse.
     * */
    async SendByObject(objData, clbck) {
        if (!this.callHelper)
            InitCallHelper();

        try {
            if (!objData)
                objData = {};

            if (!objData["method"])
                objData["method"] = "get";

            if (!objData["url"])
                objData["url"] = this.sLink;

            let lstObj = this.callHelper.request(objData);
            return this.ReturnResultAndCallback(lstObj, clbck);
        }
        catch (ex) {
            throw ex;
        }
    }

    /**
     * fonction pour l'envoi des données en mode get.
     * @param {Object}  objData les données supplémentaires pour l'envoi.
     * @param {Function} clbck fonction de callback eventuelle.
     * @returns {JSon} les données contenues dans la réponse.
     * */
    async GetAsync(objData, clbck) {
        if (!this.callHelper)
            InitCallHelper();

        try {
            let lstObj = await (objData ? this.callHelper.get(this.sLink, objData) : this.callHelper.get(this.sLink));
            return this.ReturnResultAndCallback(lstObj, clbck);
        }
        catch (ex) {
            throw ex;
        }
    }

    /**
    * fonction pour l'envoi des données en mode post.
     * @param {Object}  objData les données supplémentaires pour l'envoi.
     * @param {Function} clbck fonction de callback eventuelle.
     * @returns {JSon} les données contenues dans la réponse.
    * */
    async PostAsync (objData, clbck) {
        if (!this.callHelper)
            InitCallHelper();

        try {
            let lstObj = await (objData ? this.callHelper.post(this.sLink, objData) : this.callHelper.post(this.sLink));
            return this.ReturnResultAndCallback(lstObj, clbck);
        }
        catch (ex) {
            throw ex;
        }
    }


    /**
    * fonction pour l'envoi des données en mode post.
     * @param {Object}  objData les données supplémentaires pour l'envoi.
     * @param {Object}  objConf la configuration.
     * @param {Function} clbck fonction de callback eventuelle.
     * @returns {JSon} les données contenues dans la réponse.
    * */
    async PostAsyncWHeader(objData, objConf, clbck) {
        if (!this.callHelper)
            InitCallHelper();

        try {
            let lstObj = await (objData && objConf ? this.callHelper.post(this.sLink, objData, objConf) : this.PostAsync(objData, clbck));
            return (!objConf) ? this.ReturnResultAndCallback(lstObj, clbck) : lstObj;
        }
        catch (ex) {
            throw ex;
        }
    }

    /**
    * fonction pour commande put en http
     * @param {Object}  objData les données supplémentaires pour l'envoi.
     * @param {Function} clbck fonction de callback eventuelle.
     * @returns {JSon} les données contenues dans la réponse.
    * */
    async PutAsync(objData, clbck) {
        if (!this.callHelper)
            InitCallHelper();

        try {
            let lstObj = await (objData ? this.callHelper.put(this.sLink, objData) : this.callHelper.put(this.sLink));

            return this.ReturnResultAndCallback(lstObj, clbck);
        }
        catch (ex) {
            throw ex;
        }
    }


    /**
    * fonction pour la commande delete en http.
     * @param {Object}  objData les données supplémentaires pour l'envoi.
     * @param {Function} clbck fonction de callback eventuelle.
     * @returns {JSon} les données contenues dans la réponse.
    * */
    async DeleteAsync (objData, clbck) {
        if (!this.callHelper)
            InitCallHelper();

        try {
            let lstObj = await (objData ? this.callHelper.delete (this.sLink, objData) : this.callHelper.delete(this.sLink));
            return this.ReturnResultAndCallback(lstObj, clbck);
        }
        catch (ex) {
            throw ex;
        }

    }
}