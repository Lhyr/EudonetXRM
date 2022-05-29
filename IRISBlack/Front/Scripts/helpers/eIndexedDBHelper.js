
/**
 * Classe d'encapsulation pour les interactions avec IndexedDB.
 * */
export default class eIndexedDBHelper {

	/**
	 * Initialise l'objet avec le nom de la base de donnée et la version voulue.
	 * @param {any} DBName
	 * @param {any} DBSchema
	 * @param {any} DBVersion
	 */
	constructor(DBName, DBSchema, DBVersion = undefined) {
		this.dataBaseName = DBName;
		this.dataBaseSchema = DBSchema;
		this.dataBaseVersion = DBVersion;
	}

	/**
	 * Récupération de la base de données, avec la version,
	 * qui déclenchera peut-être un upgrade.
	 * */
	getDb() {
		if (!(window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB
			|| window.IDBTransaction || window.webkitIDBTransaction || window.msIDBTransaction
			|| window.IDBKeyRange || window.webkitIDBKeyRange || window.msIDBKeyRange))
			return;

		var db = new Dexie(this.dataBaseName);

		if (!this.dataBaseVersion || this.dataBaseVersion < 0
			|| this.dataBaseVersion < parseInt(db.verno))
			this.dataBaseVersion = parseInt(db.verno) + 1

		db.version(this.dataBaseVersion).stores(this.dataBaseSchema);

		return db;
	}

	/**
	 * Retourne une des tables de la base suivant son nom
	 * @param {any} tblName
	 */
	getObjectsFromTable(tblName) {

		if (!this.getDb())
			return;

		return this.getDb()[tblName]
	}

	/**
	 * Ajout d'un objet dans la base de données.
	 * @param {any} obj
	 */
	async addObjectToDb(tblName, obj) {
		return this.getObjectsFromTable(tblName)?.add(obj);
	}

	/**
	 * Ajout ou remplacement d'un objet dans la base de données.
	 * @param {any} tbl la table où insérer les données	 
	 * @param {any} obj les données
	 * @param {any} id paramètre optionnel - valeur de la clé primaire dans le cas d'une table sans autoincrement (ex : clé composée ou compound key) si la clé n'est pas déjà précisée comme colonne à mettre à jour dans obj
	 */
	async mergeObjectToDb(tblName, obj, id = undefined) {
		if (id)
			return this.getObjectsFromTable(tblName)?.put(obj, id);

		return this.getObjectsFromTable(tblName)?.put(obj);
	}

	/**
	 * mise à jour d'un objet dans la base de données
	 * @param {any} id
	 * @param {any} obj
	 */
	async updateObjectFromDb(tblName, id, obj) {
		return this.getObjectsFromTable(tblName)?.update(id, obj);
	}

	/**
	 * Suppression d'un objet dans la base de données
	 * @param {any} id si c'est un tableau on estime que c'est un tableau de clefs primaires et non une seule
	 */
	async deleteObjectFromDb(tblName, id) {
		if (!Array.isArray(id))
			return this.getObjectsFromTable(tblName)?.delete(id);

		return this.getObjectsFromTable(tblName)?.bulkDelete(id);
	}

	/**
	 * Suppression d'un objet dans la base de données
	 * @param {any} obj critères
	 */
	async deleteFilteredObjectsFromDb(tblName, obj) {
		let lstObj = await this.whereObjectFromDb(tblName, obj);
		let lstDescids = lstObj?.map(o => o.descid);
		return this.getObjectsFromTable(tblName)?.bulkDelete(lstDescids);
	}


	/**
	 * purge toute une table de ses données
	 * Equivalent au truncate de sql.
	 * @param {any} tblName
	 */
	async truncateObjectFromDb(tblName) {
		return this.getObjectsFromTable(tblName)?.clear();
	}

	/**
	 * Permet de filtrer les éléments d'une table.
	 * @param {any} tblName
	 * @param {any} obj
	 */
	async filterObjectFromDb(tblName, obj) {
		return this.getObjectsFromTable(tblName)?.where(obj);
	}

	/**
	 * Retourne un tableau de valeurs filtrées.
	 * Where en SQL
	 * @param {any} tblName
	 * @param {any} objFilter
	 */
	async whereObjectFromDb(tblName, objFilter) {

		let objDB = await this.filterObjectFromDb(tblName, objFilter);

		return objDB?.toArray();
	}

	/**
	 * Retourne un tableau de valeurs filtrées, à partir de valeurs données.
	 * Where en SQL
	 * @param {any} tblName
	 * @param {any} id
	 * @param {any} values
	 */
	async whereObjectFromDbWRange(tblName, id, values) {

		let objDB = await this.getObjectsFromTable(tblName)?.where(id).anyOf(values);

		return objDB?.toArray();
	}

	/**
	 * compte le nombre d'éléments avec éventuellement un filtre
	 * @param {any} tblName
	 * @param {any} objFilter
	 */
	async countObjectFromDb(tblName, objFilter) {

		let objDB;

		if (objFilter)
			objDB = await this.filterObjectFromDb(tblName, objFilter);
		else
			objDB = await this.getObjectsFromTable(tblName);

		return objDB?.count();
	}

	/**
	 * retourne le premier élément avec éventuellement un filtre
	 * @param {any} tblName
	 * @param {any} objFilter
	 */
	async firstObjectFromDb(tblName, objFilter) {

		let objDB;

		if (objFilter)
			objDB = await this.filterObjectFromDb(tblName, objFilter);
		else
			objDB = await this.getObjectsFromTable(tblName);

		return objDB?.first();
	}

	/**
	 * Intercepte les erreurs liées à IndexedDB
	 * @param {any} exception Erreur à gérer
	 */
	async manageError(exception) {
		console.log("Une erreur non gérée est survenue sur IndexedDB : " + exception?.message);
	}
}