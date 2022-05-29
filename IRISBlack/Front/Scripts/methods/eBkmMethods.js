import eIndexedDBHelper from '../helpers/eIndexedDBHelper.js?ver=803000';

/**
 * Permet d'insérer les données dans le IndexDb du navigateur
 * @param {string} database la base de données où stocker les données
 * @param {object} keys objet avec table et les clefs de la base de données
 * @param {int} version numero de version de la base
 * @returns 
 */
async function initIndexedDB(database, keys, version) {
	try {
		let db = new eIndexedDBHelper(database, keys, version);
		return db;
	}
	catch (e) {
		manageIndexedDB(e, db);
	}
}


/**
 * Ajoute des données dans la base de données locale
 * @param {any} db la base de données
 * @param {any} tbl la table où insérer les données
 * @param {any} oData les données.
 */
async function setDataDbAsync(db, tbl, oData) {
	try {
		return db.addObjectToDb(tbl, oData);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}


/**
 * Ajoute ou met à jour des données dans la base de données locale
 * @param {any} db la base de données
 * @param {any} tbl la table où insérer les données
 * @param {any} oData les données.
 * @param {any} id valeur de la clé primaire dans le cas d'une table sans autoincrement (ex : clé composée ou compound key)
 */
async function mergeDataDbAsync(db, tbl, oData, id) {
	try {
		return db.mergeObjectToDb(tbl, oData, id);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}

/**
 * Récupère les données dans la base de données locale
 * @param {any} db la base de données
 * @param {any} tbl la table où récupérer les données
 */
async function getDataDbAsync(db, tbl) {
	try {
		return db.getObjectsFromTable(tbl);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}

/**
 * Permet de filtrer les données d'une table.
 * @param {any} db la base de données
 * @param {any} tbl la table où insérer les données
 * @param {any} oData les données.
 */
async function filterDataDbAsync(db, tbl, oData) {
	try {
		return db.filterObjectFromDb(tbl, oData);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}

/**
 * Permet de retourner un jeu de données filtrées.
 * @param {any} db la base de données
 * @param {any} tbl la tabled des données
 * @param {any} oData le filtre.
 */
async function whereDataDbAsync(db, tbl, oData) {
	try {
		return db.whereObjectFromDb(tbl, oData);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}

/**
 * Permet de retourner un jeu de données filtrées.
 * @param {any} db la base de données
 * @param {any} tbl la tabled des données
 * @param {any} id la clef
 * @param {any} arValues les tableau de valeurs.
 */
async function whereDataDbAsyncWRange(db, tbl, id, arValues) {
	try {
		return db.whereObjectFromDbWRange(tbl, id, arValues);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}


/**
 * Permet de filtrer les données d'une table.
 * @param {any} db la base de données
 * @param {any} tbl la table où insérer les données
 * @param {any} oDataFilter les données.
 */
async function countDataDbAsync(db, tbl, oDataFilter) {
	try {
		return db.countObjectFromDb(tbl, oDataFilter);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}


/**
 * Permet de filtrer les données d'une table.
 * @param {any} db la base de données
 * @param {any} tbl la table où insérer les données
 * @param {any} oDataFilter les données.
 */
async function firstDataDbAsync(db, tbl, oDataFilter) {
	try {
		return db.firstObjectFromDb(tbl, oDataFilter);
	}
	catch (e) {
		manageIndexedDB(e, db);
	}	
}

/**
 * Intercepte les erreurs liées à IndexedDB
 * @param {any} exception Erreur à gérer
 * @param {any} dbOrArguments Paramètre db s'il a pu être créé, ou paramètres de connexion à la base
 */
function manageIndexedDB(exception, dbOrArguments) {
	if (dbOrArguments && typeof (dbOrArguments.manageError) == "function")
		dbOrArguments.manageError(exception);
	else {
		console.log("Une erreur est survenue sur IndexedDB : " + exception.message);
	}
}

/**
 * Permet de savoir si on doit afficher les skeletons sur les signets
 * @param {any} options objet qui permet de savoir si on affiche ou non les skeletons, récupère 
 * position de la scrollbar
 */
function reloadBkmSkeleton(options) {
	this.bkmLoaded = options.bkmLoaded;

	this.$nextTick(() => {
		this.$el.scrollLeft = options?.scrollLeft;
	})
}



/**
 * QBO - Incrémente la valeur du scroll Y
*/
function scrollSignet() {
	let signetSsroll = this.$refs.bkmScroll;
	this.scrollLeftSignet = signetSsroll.scrollLeft;
}

/**
* QBO - Récupère l'attribut 'scroll' sur le signet pour le transmettre à l'enfant et replacer la scrollBar
*/
function setScrollY() {
	let signet = this.$refs[this.signet.id];
	let signetSsroll = this.$refs.bkmScroll;
	if (signetSsroll) {
		signetSsroll.scrollLeft = signet.getAttribute('scroll')
	}
}

export { initIndexedDB, manageIndexedDB, setDataDbAsync, getDataDbAsync, mergeDataDbAsync, filterDataDbAsync, whereDataDbAsync, countDataDbAsync, firstDataDbAsync, whereDataDbAsyncWRange, reloadBkmSkeleton, scrollSignet, setScrollY };