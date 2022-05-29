
/**
 * Classe d'encapsulation pour les interactions avec IndexedDB.
 * */
export default class eIndexedDBHelper {

	/**
	 * Initialise l'objet avec le nom de la base de donn�e et la version voulue.
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
	 * R�cup�ration de la base de donn�es, avec la version,
	 * qui d�clenchera peut-�tre un upgrade.
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
	 * Ajout d'un objet dans la base de donn�es.
	 * @param {any} obj
	 */
	async addObjectToDb(tblName, obj) {
		return this.getObjectsFromTable(tblName)?.add(obj);
	}

	/**
	 * Ajout ou remplacement d'un objet dans la base de donn�es.
	 * @param {any} tbl la table o� ins�rer les donn�es	 
	 * @param {any} obj les donn�es
	 * @param {any} id param�tre optionnel - valeur de la cl� primaire dans le cas d'une table sans autoincrement (ex : cl� compos�e ou compound key) si la cl� n'est pas d�j� pr�cis�e comme colonne � mettre � jour dans obj
	 */
	async mergeObjectToDb(tblName, obj, id = undefined) {
		if (id)
			return this.getObjectsFromTable(tblName)?.put(obj, id);

		return this.getObjectsFromTable(tblName)?.put(obj);
	}

	/**
	 * mise � jour d'un objet dans la base de donn�es
	 * @param {any} id
	 * @param {any} obj
	 */
	async updateObjectFromDb(tblName, id, obj) {
		return this.getObjectsFromTable(tblName)?.update(id, obj);
	}

	/**
	 * Suppression d'un objet dans la base de donn�es
	 * @param {any} id si c'est un tableau on estime que c'est un tableau de clefs primaires et non une seule
	 */
	async deleteObjectFromDb(tblName, id) {
		if (!Array.isArray(id))
			return this.getObjectsFromTable(tblName)?.delete(id);

		return this.getObjectsFromTable(tblName)?.bulkDelete(id);
	}

	/**
	 * Suppression d'un objet dans la base de donn�es
	 * @param {any} obj crit�res
	 */
	async deleteFilteredObjectsFromDb(tblName, obj) {
		let lstObj = await this.whereObjectFromDb(tblName, obj);
		let lstDescids = lstObj?.map(o => o.descid);
		return this.getObjectsFromTable(tblName)?.bulkDelete(lstDescids);
	}


	/**
	 * purge toute une table de ses donn�es
	 * Equivalent au truncate de sql.
	 * @param {any} tblName
	 */
	async truncateObjectFromDb(tblName) {
		return this.getObjectsFromTable(tblName)?.clear();
	}

	/**
	 * Permet de filtrer les �l�ments d'une table.
	 * @param {any} tblName
	 * @param {any} obj
	 */
	async filterObjectFromDb(tblName, obj) {
		return this.getObjectsFromTable(tblName)?.where(obj);
	}

	/**
	 * Retourne un tableau de valeurs filtr�es.
	 * Where en SQL
	 * @param {any} tblName
	 * @param {any} objFilter
	 */
	async whereObjectFromDb(tblName, objFilter) {

		let objDB = await this.filterObjectFromDb(tblName, objFilter);

		return objDB?.toArray();
	}

	/**
	 * Retourne un tableau de valeurs filtr�es, � partir de valeurs donn�es.
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
	 * compte le nombre d'�l�ments avec �ventuellement un filtre
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
	 * retourne le premier �l�ment avec �ventuellement un filtre
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
	 * Intercepte les erreurs li�es � IndexedDB
	 * @param {any} exception Erreur � g�rer
	 */
	async manageError(exception) {
		console.log("Une erreur non g�r�e est survenue sur IndexedDB : " + exception?.message);
	}
}