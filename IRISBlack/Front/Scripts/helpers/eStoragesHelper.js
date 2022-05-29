/** Type de storage utilisables dans le navigateur */
export const TypeStorage = {
    SessionStorage: 1,
    LocalStorage: 2
};

/** Helper encapsulant l'utilisation des storages */
export class eStoragesHelper {
    /**
     * constructeur indiquant le type de Storage utilisé.
     * @param {TypeStorage} Type
     */
    constructor(Type) {
        this.Storage = initTypeStorage(Type);
    }

    /**
     * Retourne le bon storage suivant le type donné en paramètre.
     * @param {any} Type
     */
    initTypeStorage(Type) {
        if (Type == TypeStorage.SessionStorage)
            return Window.SessionStorage

        if (Type == TypeStorage.LocalStorage)
            return Window.LocalStorage
    }

    /** retourne le nombre d'éléments du storage. */
    getLength() {
        return this.Storage.length;
    }

    /**
     * Retourne la clef correspondant au nombre passé en paramètre.
     * @param {any} number
     */
    getKey(number) {
        this.Storage.key(number);
    }

    /**
     * retourne l'élément du storage correspondant à la clef donnée en 
     * paramètre.
     * @param {any} key
     */
    getItem(key) {
        return this.Storage.getItem(key);
    }

    /**
     * retourne l'élément du storage correspondant à la clef donnée en 
     * paramètre. Le paramètre datetimeRecordStorage est retiré s'il 
     * est présent.
     * @param {any} key
     */
    getItemWoDateTimeRecord(key) {
        let value = this.getItem(key);

        if (value.hasOwnProperty("datetimeRecordStorage"))
            delete value.datetimeRecordStorage

        return value;
    }

    /**
    * Permet d'insérer un élément dans le storage, avec clef et valeur.
    * @param {any} key
    * @param {any} value
    */
    setItem(key, value) {
        this.Storage.setItem(key, value);
    }

    /**
     * Permet d'insérer un élément dans le storage, avec clef et valeur.
     * On ajoute la date et l'heure à laquelle l'enregistrement a été effectué.
     * @param {any} key
     * @param {any} value
     */
    setItemWithDateTimeRecord(key, value) {
        value["datetimeRecordStorage"] = new Date();
        this.setItem(key, value);
    }


    /**
     * Supprime un élément déterminé par sa clef.
     * @param {any} key
     */
    removeItem(key) {
        this.Storage.removeItem(key)
    }

    /**
     * Vide le Storage.
     * */
    clear() {
        this.Storage.clear();
    }

}